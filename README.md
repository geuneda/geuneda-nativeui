# Geuneda Native UI

네이티브 UI 알림, 게임 리뷰 팝업, 메시지 토스트를 표시하는 OS 기능을 제공하는 Unity 패키지입니다.

## 개요

이 패키지는 iOS 및 Android의 네이티브 UI 기능을 Unity에서 사용할 수 있게 해줍니다.

## 주요 기능

- 네이티브 알림 다이얼로그
- 게임 리뷰 팝업
- 토스트 메시지

## 요구 사항

- Unity 2020.3 이상

## 설치 방법

### Unity Package Manager를 통한 설치

1. Unity 에디터에서 `Window` > `Package Manager`를 엽니다.
2. 좌측 상단의 `+` 버튼을 클릭하고 `Add package from git URL...`을 선택합니다.
3. 다음 URL을 입력합니다:
   ```
   https://github.com/geuneda/geuneda-nativeui.git
   ```
4. `Add` 버튼을 클릭합니다.

### manifest.json을 통한 설치

프로젝트의 `Packages/manifest.json` 파일에 다음을 추가합니다:

```json
{
  "dependencies": {
    "com.geuneda.nativeui": "https://github.com/geuneda/geuneda-nativeui.git"
  }
}
```

## 네임스페이스

```csharp
using Geuneda.NativeUI;
```

## 라이선스

MIT License
