using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;
using System;

namespace CRT
{
    internal class CRTEffectProcessor : VideoEffectProcessorBase
    {
        private readonly CRTEffect _item;
        private CRTCustomEffect? _effect;
        private ID2D1Image? _inputImage;

        public CRTEffectProcessor(IGraphicsDevicesAndContext devices, CRTEffect item) : base(devices)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public override DrawDescription Update(EffectDescription effectDescription)
        {
            if (IsPassThroughEffect || _effect is null)
                return effectDescription.DrawDescription;

            try
            {
                var frame = effectDescription.ItemPosition.Frame;
                var length = effectDescription.ItemDuration.Frame;
                var fps = effectDescription.FPS;

                UpdateEffectParameters(frame, length, fps);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CRTEffectProcessor.Update: {ex.Message}");
            }
            return effectDescription.DrawDescription;
        }

        private void UpdateEffectParameters(int frame, int length, double fps)
        {
            if (_effect == null) return;

            _effect.CanvasSize = new Vector2(
                (float)_item.CanvasWidth.GetValue(frame, length, (int)fps),
                (float)_item.CanvasHeight.GetValue(frame, length, (int)fps)
            );
            _effect.Curvature = (float)_item.Curvature.GetValue(frame, length, (int)fps);
            _effect.ChromaticAberration = (float)_item.ChromaticAberration.GetValue(frame, length, (int)fps);
            _effect.Vignette = (float)_item.Vignette.GetValue(frame, length, (int)fps);
            _effect.ScanlineIntensity = (float)_item.ScanlineIntensity.GetValue(frame, length, (int)fps);
            _effect.ScanlineDensity = (float)_item.ScanlineDensity.GetValue(frame, length, (int)fps);
            _effect.NoiseAmount = (float)_item.NoiseAmount.GetValue(frame, length, (int)fps);
            _effect.FlickerAmount = (float)_item.FlickerAmount.GetValue(frame, length, (int)fps);
            _effect.PhosphorMaskIntensity = (float)_item.PhosphorMaskIntensity.GetValue(frame, length, (int)fps);
            _effect.PhosphorMaskSize = (float)_item.PhosphorMaskSize.GetValue(frame, length, (int)fps);
            _effect.Brightness = (float)_item.Brightness.GetValue(frame, length, (int)fps);
            _effect.Contrast = (float)_item.Contrast.GetValue(frame, length, (int)fps);
            _effect.Time = (float)(frame / fps);
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            try
            {
                _effect = new CRTCustomEffect(devices);
                if (!_effect.IsEnabled)
                {
                    _effect?.Dispose();
                    _effect = null;
                    return null;
                }
                disposer.Collect(_effect);
                var output = _effect.Output;
                disposer.Collect(output);
                return output;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating CRTCustomEffect: {ex.Message}");
                _effect?.Dispose();
                _effect = null;
                return null;
            }
        }

        protected override void setInput(ID2D1Image? input)
        {
            _inputImage = input;
            _effect?.SetInput(0, input, true);
        }

        protected override void ClearEffectChain()
        {
            _effect?.SetInput(0, null, true);
        }
    }
}