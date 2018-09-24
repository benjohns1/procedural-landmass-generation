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
            }
            return null;
        }
    }
}