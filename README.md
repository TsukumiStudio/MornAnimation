# MornAnimation

## 概要

UIアニメーション管理システム。Show/Hideアニメーションをモジュール設計で柔軟に制御できるライブラリ。

## 依存関係

| 種別 | 名前 |
|------|------|
| 外部パッケージ | UniTask, TextMeshPro |
| Mornライブラリ | MornLib, MornGlobal, MornEase |
| オプション | Arbor3 |

## 使い方

### セットアップ

1. `Assets > Create > Morn > MornAnimationGlobal` でアセットを作成
2. `MornAnimationTimeSettings` を作成し設定
3. 必要に応じて `SeMixerGroup` を設定

### コンポーネント

| コンポーネント | 説明 |
|---------------|------|
| MornAnimationTarget | 複数モジュール対応 |
| MornAnimationText | テキスト文字送り |
| MornAnimationSequence | 子要素を順番にアニメーション |

### モジュール

| モジュール | 機能 |
|-----------|------|
| FadeModule | フェードイン/アウト |
| MoveUGUIModule | UGUI移動 |
| RotateModule | 回転 |
| ScaleModule | スケール |
| TextModule | テキスト文字送り |

### 基本的な使用方法

```csharp
var anim = GetComponent<MornAnimationBase>();

// 表示/非表示（非同期）
await anim.ShowAsync();
await anim.HideAsync();

// 即座に表示/非表示
anim.ShowImmediate();
anim.HideImmediate();
```
