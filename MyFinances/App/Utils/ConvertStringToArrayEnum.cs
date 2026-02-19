namespace MyFinances.App.Utils
{
    public static class ConvertStringToArrayEnum
    {
        public static List<int> Convert(string? input)
        {
            var result = new List<int>();
            if (!string.IsNullOrWhiteSpace(input))
            {
                var values = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    if (int.TryParse(value.Trim(), out var intValue))
                    {
                        result.Add(intValue);
                    }
                }
            }
            return result;
        }
    }
}
