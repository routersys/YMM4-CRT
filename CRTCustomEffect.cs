using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using System;

namespace CRT
{
    public class CRTCustomEffect : D2D1CustomShaderEffectBase
    {
        public CRTCustomEffect(IGraphicsDevicesAndContext devices) : base(Create<EffectImpl>(devices)) { }

        public Vector2 CanvasSize { set => SetValue((int)EffectImpl.Properties.CanvasSize, value); }
        public float Curvature { set => SetValue((int)EffectImpl.Properties.Curvature, value); }
        public float ChromaticAberration { set => SetValue((int)EffectImpl.Properties.ChromaticAberration, value); }
        public float Vignette { set => SetValue((int)EffectImpl.Properties.Vignette, value); }
        public float ScanlineIntensity { set => SetValue((int)EffectImpl.Properties.ScanlineIntensity, value); }
        public float ScanlineDensity { set => SetValue((int)EffectImpl.Properties.ScanlineDensity, value); }
        public float NoiseAmount { set => SetValue((int)EffectImpl.Properties.NoiseAmount, value); }
        public float FlickerAmount { set => SetValue((int)EffectImpl.Properties.FlickerAmount, value); }
        public float PhosphorMaskIntensity { set => SetValue((int)EffectImpl.Properties.PhosphorMaskIntensity, value); }
        public float PhosphorMaskSize { set => SetValue((int)EffectImpl.Properties.PhosphorMaskSize, value); }
        public float Brightness { set => SetValue((int)EffectImpl.Properties.Brightness, value); }
        public float Contrast { set => SetValue((int)EffectImpl.Properties.Contrast, value); }
        public float Time { set => SetValue((int)EffectImpl.Properties.Time, value); }

        [CustomEffect(1)]
        class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
        {
            private ConstantBuffer _constants;

            [CustomEffectProperty(PropertyType.Vector2, (int)Properties.CanvasSize)]
            public Vector2 CanvasSize { set { _constants.CanvasSize = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Curvature)]
            public float Curvature { set { _constants.Curvature = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.ChromaticAberration)]
            public float ChromaticAberration { set { _constants.ChromaticAberration = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Vignette)]
            public float Vignette { set { _constants.Vignette = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.ScanlineIntensity)]
            public float ScanlineIntensity { set { _constants.ScanlineIntensity = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.ScanlineDensity)]
            public float ScanlineDensity { set { _constants.ScanlineDensity = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.NoiseAmount)]
            public float NoiseAmount { set { _constants.NoiseAmount = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.FlickerAmount)]
            public float FlickerAmount { set { _constants.FlickerAmount = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.PhosphorMaskIntensity)]
            public float PhosphorMaskIntensity { set { _constants.PhosphorMaskIntensity = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.PhosphorMaskSize)]
            public float PhosphorMaskSize { set { _constants.PhosphorMaskSize = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Brightness)]
            public float Brightness { set { _constants.Brightness = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Contrast)]
            public float Contrast { set { _constants.Contrast = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Time)]
            public float Time { set { _constants.Time = value; UpdateConstants(); } }

            public EffectImpl() : base(ShaderResourceLoader.GetShaderResource("CRTShader.cso")) { }

            protected override void UpdateConstants()
            {
                drawInformation?.SetPixelShaderConstantBuffer(_constants);
            }

            public override void MapInputRectsToOutputRect(RawRect[] inputRects, RawRect[] inputOpaqueSubRects, out RawRect outputRect, out RawRect outputOpaqueSubRect)
            {
                if (inputRects == null || inputRects.Length != 1)
                    throw new ArgumentException("InputRects must be non-null and have length of 1", nameof(inputRects));

                var input = inputRects[0];
                int expansion = 2000;

                outputRect = new RawRect(
                    input.Left - expansion,
                    input.Top - expansion,
                    input.Right + expansion,
                    input.Bottom + expansion);

                outputOpaqueSubRect = new RawRect(0, 0, 0, 0);
            }

            public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
            {
                if (inputRects == null || inputRects.Length != 1)
                    throw new ArgumentException("InputRects must be non-null and have length of 1", nameof(inputRects));

                int expansion = 2000;

                inputRects[0] = new RawRect(
                    outputRect.Left - expansion,
                    outputRect.Top - expansion,
                    outputRect.Right + expansion,
                    outputRect.Bottom + expansion);
            }

            [StructLayout(LayoutKind.Explicit, Size = 64)]
            struct ConstantBuffer
            {
                [FieldOffset(0)] public Vector2 CanvasSize;
                [FieldOffset(8)] public float Curvature;
                [FieldOffset(12)] public float ChromaticAberration;
                [FieldOffset(16)] public float Vignette;
                [FieldOffset(20)] public float ScanlineIntensity;
                [FieldOffset(24)] public float ScanlineDensity;
                [FieldOffset(28)] public float NoiseAmount;
                [FieldOffset(32)] public float FlickerAmount;
                [FieldOffset(36)] public float PhosphorMaskIntensity;
                [FieldOffset(40)] public float PhosphorMaskSize;
                [FieldOffset(44)] public float Brightness;
                [FieldOffset(48)] public float Contrast;
                [FieldOffset(52)] public float Time;
                [FieldOffset(56)] public float Padding1;
                [FieldOffset(60)] public float Padding2;
            }

            public enum Properties : int
            {
                CanvasSize = 0,
                Curvature = 1,
                ChromaticAberration = 2,
                Vignette = 3,
                ScanlineIntensity = 4,
                ScanlineDensity = 5,
                NoiseAmount = 6,
                FlickerAmount = 7,
                PhosphorMaskIntensity = 8,
                PhosphorMaskSize = 9,
                Brightness = 10,
                Contrast = 11,
                Time = 12
            }
        }
    }
}