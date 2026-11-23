using System;
using System.IO;
using Xunit;

namespace RPG2D.Tests
{
    public sealed class TestProjectConfiguration
    {
        [Fact]
        public void GodotProject_DeclaresExpectedEngineAndDotNetFeature()
        {
            string projectPath = Path.Combine(AppContext.BaseDirectory, "project.godot");
            string projectText = File.ReadAllText(projectPath);

            Assert.Contains("config/features=PackedStringArray(\"4.7\", \"C#\"", projectText);
            Assert.Contains("project/assembly_name=\"RPG2D\"", projectText);
        }
    }
}
