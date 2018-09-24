namespace NoiseGenerator
{
    public static class FilterFactory
    {
        public static IFilter CreateFilter(FilterSettings settings)
        {
            switch (settings.filterType)
            {
                case FilterSettings.FilterType.Perlin:
                    return new Filters.Perlin(settings.perlinSettings);
                case FilterSettings.FilterType.Curve:
                    return new Filters.Curve(settings.curveSettings);
                case FilterSettings.FilterType.Constant:
                    return new Filters.Constant(settings.constantSettings);
            }
            throw new System.Exception("Unknown filter type");
        }
    }
}