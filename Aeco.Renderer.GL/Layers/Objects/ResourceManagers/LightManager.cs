namespace Aeco.Renderer.GL;

public class LightManager : ResourceManagerBase<Light, LightData, LightResourceBase>, IGLRenderLayer
{
    private Query<LightData, TransformMatricesDirty> _q = new();
    private Stack<int> _lightIds = new();
    private int _maxId;

    public void OnRender(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref var lightData = ref context.Acquire<LightData>(id);
            UpdateLightTransform(context, id, ref lightData.Parameters);
        }
    }

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
                pars.ConeAnglesOrAreaSize.X = spotLight.InnerConeAngle;
                pars.ConeAnglesOrAreaSize.Y = spotLight.OuterConeAngle;
                break;
            case AreaLightResource areaLight:
                data.Category = LightCategory.Area;
                pars.ConeAnglesOrAreaSize = areaLight.AreaSize;
                break;
            }
        }

        pars.Category = (float)data.Category;
    }

    private void UpdateLightTransform(IDataLayer<IComponent> context, Guid id, ref LightParameters pars)
    {
        ref var worldAxes = ref context.Acquire<WorldAxes>(id);
        pars.Position = context.Acquire<WorldPosition>(id).Value;
        pars.Direction = worldAxes.Forward;
        pars.Up = worldAxes.Up;
    }

    protected override void Uninitialize(IDataLayer<IComponent> context, Guid id, in Light light, in LightData data)
    {
        _lightIds.Push(data.Id);
    }
}