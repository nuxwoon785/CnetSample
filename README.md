# Multi-head ROI 검사 샘플

모바일넷을 사용해 ROI별 OK/NG를 판정하는 파이프라인 예제입니다. WinForms 기반으로 ROI를 추가/수정/삭제하고 데이터셋을 만들며, 파이썬으로 학습한 ONNX 모델을 불러와 추론까지 수행할 수 있습니다.

## 프로젝트 구성
- **WinForms (C#)**: `Form1`에서 이미지 로드, ROI 편집, 데이터셋 저장, ONNX 추론 실행 기능을 제공합니다.
- **머신러닝 학습 스크립트 (Python)**: `ml/train_multiroi_mobilenet.py`에서 MobileNet v3 기반 멀티헤드 분류기를 학습하고 ONNX로 내보냅니다.

## C# 애플리케이션 사용 방법
1. **이미지 로드**: `이미지 로드` 버튼으로 검사할 이미지를 불러옵니다. ROI는 마우스 드래그로 생성하며, 리스트에서 선택 후 위치/크기를 숫자로 수정하거나 `ROI 삭제` 버튼으로 제거할 수 있습니다.
2. **데이터셋 저장**: 데이터셋 경로(`dataset` 기본값)를 지정한 뒤 `OK 저장` 또는 `NG 저장`을 누르면 각 ROI가 `dataset/ROI명/OK|NG` 구조로 잘려 저장됩니다.
3. **모델 로드 및 추론**: 입력 크기(기본 224x224)를 설정하고 `ONNX 모델 로드` 버튼으로 학습된 모델을 불러온 다음 `추론 실행`을 누르면 ROI별 OK/NG 점수와 판정이 테이블에 표시됩니다.

## 파이썬 학습 파이프라인
1. 데이터셋 구조는 C# 프로그램이 생성하는 폴더와 동일합니다: `dataset/<ROI명>/OK/*.png`, `dataset/<ROI명>/NG/*.png`.
2. 학습 실행 예시:
   ```bash
   cd ml
   python train_multiroi_mobilenet.py --dataset ../dataset --epochs 5 --batch 8 --image_size 224 --output ../models/multihead.onnx
   ```
3. 학습이 끝나면 `models/multihead.onnx`가 생성됩니다. 이 파일을 C# 애플리케이션에서 로드하면 ROI별로 동일한 헤드를 공유하는 멀티헤드 분류를 수행합니다.

## 주의 사항
- ONNX Runtime(.NET)을 NuGet에서 복원해야 합니다. `packages.config`에 `Microsoft.ML.OnnxRuntime` 1.17.0이 포함되어 있습니다.
- WinForms 프로젝트만 포함하며 WPF는 사용하지 않았습니다.
- ROI 드래그/수정 시 이미지 좌표계를 사용하므로 원본 해상도를 기준으로 학습 및 추론이 일치합니다.
