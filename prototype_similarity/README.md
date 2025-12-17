# Prototype-based Defect Similarity

불량 마스크 예제처럼 **지원(Support) 이미지**에서 추출한 결함 특징을 **쿼리(Query) 이미지**의 각 픽셀 특징과 비교하는 프로토타입 매칭 파이프라인입니다. 동일한 컨셉을 파이썬과 C# 코드로 모두 담았습니다.

## 폴더 구조
- `python/`: NumPy 기반의 레퍼런스 구현 및 데모.
- `csharp/DefectPrototypeConsole/`: .NET 6 콘솔 앱. 동일한 계산을 C#으로 재현합니다.

## 파이썬 사용법
```bash
pip install numpy
python prototype_pipeline.py
```
출력 예시는 프로토타입 벡터의 크기와 쿼리 유사도 맵의 통계치를 보여 줍니다.

## C# 사용법
1. .NET 6 SDK를 설치한 환경에서 다음 명령을 실행합니다.
   ```bash
   dotnet build
   dotnet run --project csharp/DefectPrototypeConsole
   ```
2. 프로그램은 무작위 피처맵/마스크를 생성해 프로토타입과 유사도 맵 통계를 출력합니다.

## 파이프라인 개요
1. **지원 이미지 → CNN Backbone → Defect Feature Map**: CNN에서 (C×H×W) 피처를 추출.
2. **마스크 영역만 평균 → 불량 Feature Prototype**: 결함 마스크가 1인 위치의 피처만 평균해 C차원 프로토타입을 만듭니다.
3. **쿼리 이미지 → CNN Backbone → Feature Map**: 동일한 백본으로 쿼리 피처맵 생성.
4. **Prototype 간 유사도 계산 → Pixel-wise Similarity Map**: 각 (x,y) 위치의 피처와 프로토타입 간 코사인 유사도를 계산해 H×W 맵을 얻습니다.

두 언어 모두 위의 단계가 함수 단위로 분리되어 있어, 실제 CNN 백본에서 나온 피처맵만 끼워 넣으면 바로 사용할 수 있습니다.
