using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace CRT
{
    [VideoEffect("ブラウン管", ["描画", "レトロ"], ["crt", "ブラウン管", "走査線", "scanline", "レトロ"], IsAviUtlSupported = false)]
    public class CRTEffect : VideoEffectBase
    {
        public override string Label => "ブラウン管";

        [Display(GroupName = "キャンバス設定", Name = "幅", Description = "描画対象のキャンバス幅を指定します。", Order = 1)]
        [AnimationSlider("F0", "px", 1, 7680)]
        public Animation CanvasWidth { get; } = new(1920, 1, 7680);

        [Display(GroupName = "キャンバス設定", Name = "高さ", Description = "描画対象のキャンバス高を指定します。", Order = 2)]
        [AnimationSlider("F0", "px", 1, 4320)]
        public Animation CanvasHeight { get; } = new(1080, 1, 4320);

        [Display(GroupName = "画面の歪み", Name = "歪曲", Description = "ブラウン管の画面の丸みをシミュレートします。", Order = 10)]
        [AnimationSlider("F3", "", 0.0, 0.5)]
        public Animation Curvature { get; } = new(0.15, 0.0, 0.5);

        [Display(GroupName = "画面の歪み", Name = "色収差", Description = "画面端の色ズレをシミュレートします。", Order = 11)]
        [AnimationSlider("F3", "", 0.0, 5.0)]
        public Animation ChromaticAberration { get; } = new(0.8, 0.0, 5.0);

        [Display(GroupName = "画面の歪み", Name = "ビネット", Description = "画面端の暗さを調整します。", Order = 12)]
        [AnimationSlider("F2", "", 0.0, 1.5)]
        public Animation Vignette { get; } = new(0.3, 0.0, 1.5);

        [Display(GroupName = "走査線", Name = "強さ", Description = "走査線（スキャンライン）の強さを調整します。", Order = 20)]
        [AnimationSlider("F2", "", 0.0, 1.0)]
        public Animation ScanlineIntensity { get; } = new(0.4, 0.0, 1.0);

        [Display(GroupName = "走査線", Name = "密度", Description = "走査線の密度を調整します。値が大きいほど線が細かくなります。", Order = 21)]
        [AnimationSlider("F1", "", 0.5, 3.0)]
        public Animation ScanlineDensity { get; } = new(1.0, 0.5, 3.0);

        [Display(GroupName = "アナログノイズ", Name = "ノイズ量", Description = "映像に乗るノイズの量を調整します。", Order = 30)]
        [AnimationSlider("F2", "", 0.0, 0.5)]
        public Animation NoiseAmount { get; } = new(0.03, 0.0, 0.5);

        [Display(GroupName = "アナログノイズ", Name = "ちらつき", Description = "画面全体のちらつきの強さを調整します。", Order = 31)]
        [AnimationSlider("F2", "", 0.0, 0.3)]
        public Animation FlickerAmount { get; } = new(0.02, 0.0, 0.3);

        [Display(GroupName = "リン光マスク", Name = "強さ", Description = "RGBリン光マスクの強さを調整します。", Order = 32)]
        [AnimationSlider("F2", "", 0.0, 1.0)]
        public Animation PhosphorMaskIntensity { get; } = new(0.5, 0.0, 1.0);

        [Display(GroupName = "リン光マスク", Name = "サイズ", Description = "リン光マスクのピクセルサイズを調整します。", Order = 33)]
        [AnimationSlider("F1", "px", 1.0, 8.0)]
        public Animation PhosphorMaskSize { get; } = new(3.0, 1.0, 8.0);

        [Display(GroupName = "輝度調整", Name = "明度", Description = "全体の明度を調整します。", Order = 40)]
        [AnimationSlider("F2", "", 0.1, 2.0)]
        public Animation Brightness { get; } = new(1.1, 0.1, 2.0);

        [Display(GroupName = "輝度調整", Name = "コントラスト", Description = "コントラストを調整します。", Order = 41)]
        [AnimationSlider("F2", "", 0.1, 3.0)]
        public Animation Contrast { get; } = new(1.2, 0.1, 3.0);

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new CRTEffectProcessor(devices, this);
        }

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() =>
        [
            CanvasWidth, CanvasHeight, Curvature, ChromaticAberration, Vignette,
            ScanlineIntensity, ScanlineDensity, NoiseAmount, FlickerAmount,
            PhosphorMaskIntensity, PhosphorMaskSize, Brightness, Contrast
        ];
    }
}