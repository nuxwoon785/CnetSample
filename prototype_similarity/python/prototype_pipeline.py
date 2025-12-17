"""Prototype similarity pipeline for defect segmentation.

This module mirrors the two-step prototype pipeline from the prompt:
1) Build a prototype vector from a support image's defect region.
2) Compare a query image's feature map against the prototype pixel-wise.

The implementation is intentionally lightweight and uses NumPy so it can run
without GPU support. The code is organized to match the conceptual blocks in
the prompt (backbone -> feature map -> prototype -> similarity map).
"""

from __future__ import annotations

import numpy as np


def l2_normalize(x: np.ndarray, axis: int | tuple[int, ...]) -> np.ndarray:
    """Normalize along the given axis to unit L2 norm."""
    norm = np.linalg.norm(x, axis=axis, keepdims=True) + 1e-8
    return x / norm


def build_prototype(feature_map: np.ndarray, defect_mask: np.ndarray) -> np.ndarray:
    """Compute the defect prototype vector from a support feature map.

    Args:
        feature_map: Feature tensor of shape (C, H, W) produced by the CNN backbone.
        defect_mask: Binary mask of shape (H, W) where 1 marks the defective region.

    Returns:
        Prototype vector of shape (C,), averaged over masked pixels.
    """
    if feature_map.ndim != 3:
        raise ValueError("feature_map must have shape (C, H, W)")
    if defect_mask.ndim != 2:
        raise ValueError("defect_mask must have shape (H, W)")

    c, h, w = feature_map.shape
    if defect_mask.shape != (h, w):
        raise ValueError("defect_mask must match spatial dims of feature_map")

    mask = defect_mask.astype(bool)
    if not mask.any():
        raise ValueError("defect_mask must contain at least one positive pixel")

    masked = feature_map[:, mask]  # (C, N)
    prototype = masked.mean(axis=1)
    return prototype


def similarity_map(query_feature_map: np.ndarray, prototype: np.ndarray) -> np.ndarray:
    """Compute pixel-wise cosine similarity between query features and prototype.

    Args:
        query_feature_map: Tensor of shape (C, H, W) from the query image.
        prototype: Prototype vector of shape (C,).

    Returns:
        Similarity map of shape (H, W) with values in [-1, 1].
    """
    if query_feature_map.ndim != 3:
        raise ValueError("query_feature_map must have shape (C, H, W)")
    if prototype.ndim != 1:
        raise ValueError("prototype must have shape (C,)")

    c, h, w = query_feature_map.shape
    if prototype.shape[0] != c:
        raise ValueError("prototype channel size must match query feature channels")

    # Normalize features and prototype for cosine similarity.
    proto_norm = l2_normalize(prototype, axis=0)
    features = l2_normalize(query_feature_map, axis=0)

    # Cosine similarity for each spatial location: dot(proto, feature).
    similarity = (proto_norm[:, None, None] * features).sum(axis=0)
    return similarity


def demo() -> None:
    """Small demonstration with fake data to visualize the pipeline."""
    rng = np.random.default_rng(0)

    # Step 1: support image -> feature map -> prototype
    support_feature_map = rng.normal(size=(64, 8, 8)).astype(np.float32)
    defect_mask = np.zeros((8, 8), dtype=np.uint8)
    defect_mask[2:5, 3:6] = 1  # toy defective region
    prototype = build_prototype(support_feature_map, defect_mask)

    # Step 2: query image -> feature map -> similarity map
    query_feature_map = rng.normal(size=(64, 8, 8)).astype(np.float32)
    sim_map = similarity_map(query_feature_map, prototype)

    print("Prototype shape:", prototype.shape)
    print("Similarity map stats -> min: %.3f max: %.3f mean: %.3f" % (
        sim_map.min(), sim_map.max(), sim_map.mean()))


if __name__ == "__main__":
    demo()
