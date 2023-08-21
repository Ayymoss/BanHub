namespace BanHub.WebCore.Client.Components;

partial class TomatoButton
{
    private readonly List<Tomato> _tomatoes = new();

    private void DropTomatoes()
    {
        for (var i = 0; i < 5; i++)
        {
            var size = Random.Shared.Next(20, 50);
            var isRotten = Random.Shared.NextDouble() < 0.2;
            var rotationDirection = Random.Shared.Next(0, 2) == 0 ? "clockwise" : "counter-clockwise";
            var rotationSpeed = 0.4 + 1.6 * Random.Shared.NextDouble();

            _tomatoes.Add(new Tomato
            {
                Id = Guid.NewGuid().ToString(),
                Size = size,
                IsRotten = isRotten,
                RotationDirection = rotationDirection,
                RotationSpeed = rotationSpeed,
                FallSpeed = (int)(10.0 - 5.0 * size / 50.0),
                LeftPosition = Random.Shared.Next(100),
                TopPosition = -Random.Shared.Next(10, 20)
            });
        }
    }

    private class Tomato
    {
        public string Id { get; set; }
        public int Size { get; set; }
        public bool IsRotten { get; set; }
        public string RotationDirection { get; set; }
        public double RotationSpeed { get; set; }
        public int FallSpeed { get; set; }
        public int LeftPosition { get; set; }
        public int TopPosition { get; set; }
    }
}
