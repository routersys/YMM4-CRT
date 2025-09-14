cbuffer Constants : register(b0)
{
    float2 canvasSize;
    float curvature;
    float chromaticAberration;
    
    float vignette;
    float scanlineIntensity;
    float scanlineDensity;
    float noiseAmount;
    
    float flickerAmount;
    float phosphorMaskIntensity;
    float phosphorMaskSize;
    float brightness;
    
    float contrast;
    float time;
    float padding1;
    float padding2;
};

Texture2D<float4> InputTexture : register(t0);
SamplerState InputSampler : register(s0);

float hash(float2 p)
{
    p = 50.0 * frac(p * 0.3183099 + float2(0.71, 0.113));
    return -1.0 + 2.0 * frac(p.x * p.y * (p.x + p.y));
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(hash(i + float2(0.0, 0.0)),
                     hash(i + float2(1.0, 0.0)), u.x),
                lerp(hash(i + float2(0.0, 1.0)),
                     hash(i + float2(1.0, 1.0)), u.x), u.y);
}

float2 canvas_to_normalized(float2 uv)
{
    float2 pixel_pos = uv * canvasSize;
    float2 center = canvasSize * 0.5;
    float2 centered_pos = pixel_pos - center;
    
    float max_radius = min(canvasSize.x, canvasSize.y) * 0.5;
    float2 normalized = centered_pos / max_radius;
    
    return normalized * 0.5 + 0.5;
}

float2 normalized_to_canvas(float2 normalized_uv)
{
    float2 centered = (normalized_uv - 0.5) * 2.0;
    
    float max_radius = min(canvasSize.x, canvasSize.y) * 0.5;
    float2 centered_pos = centered * max_radius;
    
    float2 center = canvasSize * 0.5;
    float2 pixel_pos = centered_pos + center;
    
    return pixel_pos / canvasSize;
}

float2 barrel_distort(float2 uv, float strength)
{
    if (strength <= 0.0)
        return uv;
        
    float2 normalized = canvas_to_normalized(uv);
    float2 p = normalized - 0.5;
    float r2 = dot(p, p);
    float distortion = 1.0 + r2 * strength;
    float2 distorted_normalized = 0.5 + p * distortion;
    
    return normalized_to_canvas(distorted_normalized);
}

float3 apply_chromatic_aberration(float2 uv, float strength)
{
    if (strength <= 0.0)
    {
        float2 distorted_uv = barrel_distort(uv, curvature);
        float4 color = InputTexture.Sample(InputSampler, distorted_uv);
        return color.rgb;
    }
    
    float2 normalized = canvas_to_normalized(uv);
    float2 center = float2(0.5, 0.5);
    float2 offset = (normalized - center) * strength * 0.01;
    
    float2 uv_r_norm = center + (normalized - center + offset);
    float2 uv_g_norm = normalized;
    float2 uv_b_norm = center + (normalized - center - offset);
    
    float2 uv_r = barrel_distort(normalized_to_canvas(uv_r_norm), curvature);
    float2 uv_g = barrel_distort(uv, curvature);
    float2 uv_b = barrel_distort(normalized_to_canvas(uv_b_norm), curvature);
    
    float r = InputTexture.Sample(InputSampler, uv_r).r;
    float g = InputTexture.Sample(InputSampler, uv_g).g;
    float b = InputTexture.Sample(InputSampler, uv_b).b;
    
    return float3(r, g, b);
}

float get_scanline_intensity(float2 screen_pos, float intensity, float density)
{
    if (intensity <= 0.0)
        return 1.0;
        
    float line_freq = density * 3.14159 * (canvasSize.y / 1080.0);
    float scanline = sin(screen_pos.y * line_freq);
    scanline = scanline * 0.5 + 0.5;
    scanline = pow(scanline, 0.8);
    
    return 1.0 - (scanline * intensity * 0.6);
}

float3 apply_phosphor_mask(float3 color, float2 screen_pos, float intensity, float mask_size)
{
    if (intensity <= 0.0)
        return color;
        
    float3 mask = float3(1.0, 1.0, 1.0);
    float pixel_x = screen_pos.x / mask_size;
    int phase = int(floor(pixel_x)) % 3;
    
    float fade = intensity * 0.8;
    
    if (phase == 0)
        mask = float3(1.0, 1.0 - fade, 1.0 - fade);
    else if (phase == 1)
        mask = float3(1.0 - fade, 1.0, 1.0 - fade);
    else
        mask = float3(1.0 - fade, 1.0 - fade, 1.0);
    
    return color * mask;
}

float get_vignette_factor(float2 uv, float intensity)
{
    if (intensity <= 0.0)
        return 1.0;
        
    float2 normalized = canvas_to_normalized(uv);
    float2 center_dist = (normalized - 0.5) * 2.0;
    float vignette_factor = 1.0 - dot(center_dist, center_dist) * intensity * 0.4;
    return saturate(vignette_factor);
}

float get_flicker_factor(float time_val, float intensity)
{
    if (intensity <= 0.0)
        return 1.0;
        
    float flicker_freq = 0.1 + sin(time_val * 7.3) * 0.05;
    float flicker = 1.0 - (sin(time_val * 60.0 * flicker_freq) * 0.5 + 0.5) * intensity;
    return max(flicker, 0.7);
}

float3 apply_brightness_contrast(float3 color, float bright, float cont)
{
    color = saturate((color - 0.5) * cont + 0.5);
    color = saturate(color * bright);
    return color;
}

float4 main(float4 pos : SV_POSITION, float4 posScene : SCENE_POSITION, float4 uv0 : TEXCOORD0) : SV_TARGET
{
    float2 uv = uv0.xy;
    
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
        return float4(0.0, 0.0, 0.0, 0.0);
    
    float3 color = apply_chromatic_aberration(uv, chromaticAberration);
    float alpha = InputTexture.Sample(InputSampler, barrel_distort(uv, curvature)).a;
    
    color = apply_brightness_contrast(color, brightness, contrast);
    
    float vignette_factor = get_vignette_factor(uv, vignette);
    color *= vignette_factor;
    
    float scanline_factor = get_scanline_intensity(posScene.xy, scanlineIntensity, scanlineDensity);
    color *= scanline_factor;
    
    color = apply_phosphor_mask(color, pos.xy, phosphorMaskIntensity, phosphorMaskSize);
    
    if (noiseAmount > 0.0)
    {
        float2 noise_coord = uv * 1000.0 + float2(time * 100.0, time * 137.0);
        float noise_val = noise(noise_coord) * noiseAmount * 0.1;
        color += noise_val;
    }
    
    float flicker_factor = get_flicker_factor(time, flickerAmount);
    color *= flicker_factor;
    
    color = saturate(color);
    
    return float4(color, alpha);
}