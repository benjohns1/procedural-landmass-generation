namespace NoiseGenerator
{
    public static class FilterFactory
    {
        public static Filter CreateFilter(FilterSettings settings)
        {
            switch (settings.filterType)
            {
                case FilterSettings.FilterType.Perlin:
                    return new Filters.Perlin(settings.perlinSettings);
                case FilterSettings.FilterType.Constant:
                    return new Filters.Constant(settings.constantSettings);
                case FilterSettings.FilterType.CellularStep:
                    return new Filters.CellularStep(settings.cellularSettings);
            }
            throw new System.Exception("Unknown filter type");
        }
    }
}