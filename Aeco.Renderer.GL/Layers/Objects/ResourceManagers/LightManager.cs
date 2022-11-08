namespace Aeco.Renderer.GL;

public class LightManager : ResourceManagerBase<Light, LightData, LightResourceBase>
{
    private Stack<int> _lightIds = new();
    private int _maxId;

    protected unsafe override void Initialize(
        IDataLayer<IComponent> context, Guid id, ref Light light, ref LightData data, bool updating)
    {
        ref var pars = ref data.Parameters;

        if (!updating) {
            if (!_lightIds.TryPop(out var lightId)) {
                lightId = _maxId++;
            }
            data.Id = lightId;
        }

        var lightRes = light.Resource;
        pars.Color = lightRes.Color;

        if (lightRes is AmbientLightResource) {
            data.Category = LightCategory.Ambient;
        }
        else if (lightRes is DirectionalLightResource) {
            data.Category = LightCategory.Directional;
        }
        else if (lightRes is AttenuateLightResourceBase attLight) {
            pars.AttenuationConstant = attLight.AttenuationConstant;
            pars.AttenuationLinear = attLight.AttenuationLinear;
            pars.AttenuationQuadratic = attLight.AttenuationQuadratic;

            switch (attLight) {
            case PointLightResource:
                data.Category = LightCategory.Point;
                break;
            case SpotLightResource spotLight:
                data.Category = LightCategory.Spot;
                pars.ConeCutoffsOrAreaSize.X = MathF.Cos(spotLight.InnerConeAngle / 180f * MathF.PI);
                pars.ConeCutoffsOrAreaSize.Y = MathF.Cos(spotLight.OuterConeAngle / 180f * MathF.PI);
                break;
            case AreaLightResource areaLight:
                data.Category = LightCategory.Area;
                pars.ConeCutoffsOrAreaSize = areaLight.AreaSize;
                break;
            }
        }

        pars.Category = (float)data.Category;
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Light light, in LightData data)
    {
        _lightIds.Push(data.Id);
    }
}