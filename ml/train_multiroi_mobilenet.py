"""
모바일넷 v3 기반으로 ROI별 OK/NG를 학습한 뒤 ONNX 포맷으로 내보내는 예제.
멀티헤드 구조라 ROI마다 독립적인 분류 헤드가 붙는다.
"""
import argparse
import os
from pathlib import Path
from typing import List

import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, Dataset
from torchvision import models, transforms
from PIL import Image


class RoiFolder(Dataset):
    def __init__(self, root: Path, roi_names: List[str], image_size: int = 224):
        self.samples = []
        self.transform = transforms.Compose(
            [transforms.Resize((image_size, image_size)), transforms.ToTensor()]
        )
        for roi in roi_names:
            for label in ["OK", "NG"]:
                folder = root / roi / label
                if not folder.exists():
                    continue
                for file in folder.glob("*.png"):
                    self.samples.append((file, roi, 0 if label == "OK" else 1))

    def __len__(self):
        return len(self.samples)

    def __getitem__(self, idx):
        path, roi, target = self.samples[idx]
        img = Image.open(path).convert("RGB")
        return self.transform(img), target, roi


def build_model(num_heads: int):
    backbone = models.mobilenet_v3_small(weights=models.MobileNet_V3_Small_Weights.DEFAULT)
    backbone.classifier = nn.Identity()
    feature_dim = 576
    heads = nn.ModuleList([nn.Linear(feature_dim, 2) for _ in range(num_heads)])

    class MultiHead(nn.Module):
        def __init__(self, encoder, heads):
            super().__init__()
            self.encoder = encoder
            self.heads = heads

        def forward(self, x):
            feat = self.encoder(x)
            outputs = [head(feat) for head in self.heads]
            return outputs

    return MultiHead(backbone, heads)


def train_epoch(model, loader, roi_to_idx, device, optimizer, criterion):
    model.train()
    total_loss = 0.0
    for images, targets, rois in loader:
        images = images.to(device)
        targets = targets.to(device)
        optimizer.zero_grad()
        outputs = model(images)
        losses = []
        for i, roi in enumerate(rois):
            head = roi_to_idx[roi]
            losses.append(criterion(outputs[head][i].unsqueeze(0), targets[i].unsqueeze(0)))
        loss = sum(losses)
        loss.backward()
        optimizer.step()
        total_loss += loss.item()
    return total_loss / max(len(loader), 1)


def export_onnx(model, roi_names: List[str], path: Path, image_size: int = 224):
    model.eval()
    dummy = torch.randn(1, 3, image_size, image_size)
    output_names = [f"{roi}_head" for roi in roi_names]
    torch.onnx.export(
        model,
        dummy,
        path,
        input_names=["input"],
        output_names=output_names,
        opset_version=13,
        dynamic_axes={"input": {0: "batch"}},
    )


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", type=Path, default=Path("../dataset"))
    parser.add_argument("--epochs", type=int, default=3)
    parser.add_argument("--batch", type=int, default=8)
    parser.add_argument("--image_size", type=int, default=224)
    parser.add_argument("--output", type=Path, default=Path("../models/multihead.onnx"))
    args = parser.parse_args()

    roi_names = [d.name for d in args.dataset.iterdir() if d.is_dir()]
    if not roi_names:
        raise SystemExit("ROI 폴더가 없습니다. dataset/ROI_NAME/OK,NG 구조를 확인하세요.")

    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    ds = RoiFolder(args.dataset, roi_names, args.image_size)
    loader = DataLoader(ds, batch_size=args.batch, shuffle=True)

    model = build_model(len(roi_names)).to(device)
    optimizer = optim.Adam(model.parameters(), lr=1e-3)
    criterion = nn.CrossEntropyLoss()
    roi_to_idx = {name: i for i, name in enumerate(roi_names)}

    os.makedirs(args.output.parent, exist_ok=True)
    for epoch in range(args.epochs):
        loss = train_epoch(model, loader, roi_to_idx, device, optimizer, criterion)
        print(f"epoch {epoch+1}: loss={loss:.4f}")

    export_onnx(model, roi_names, args.output, args.image_size)
    print(f"ONNX saved to {args.output}")


if __name__ == "__main__":
    main()
