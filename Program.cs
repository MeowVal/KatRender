using Brutal;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.Strings;
using Brutal.VulkanApi;

// Vulkan packs versions as major << 22 | minor << 12 | patch.
const int API_VERSION_1_3 = 1 << 22 | 3 << 12;
static string FormatVersion(int version) => $"{(version >> 22) & 0x7F}.{(version >> 12) & 0x3FF}.{version & 0xFFF}";

// MoltenVK is a "portability" driver: macOS only lists it if the instance opts in to portability enumeration.
var extensions = new HashSet<string>();
var flags = VkInstanceCreateFlags.None;
if (Platform.IsMacOS)
{
    extensions.Add(VK.KHR_PORTABILITY_ENUMERATION_EXTENSION_NAME);
    flags |= VkInstanceCreateFlags.EnumeratePortabilityBitKHR;
}

// Everything else hangs off the instance, so it lives at the top level and is disposed after Run returns.
using var instance = Vulkan.CreateInstance(
    new Instance.CreateInfo
    {
        AppInfo = new VkApplicationInfo { ApiVersion = API_VERSION_1_3 },
        Flags = flags,
        EnabledExtensions = extensions,
        EnabledLayers = [], // Set VK_INSTANCE_LAYERS=VK_LAYER_KHRONOS_validation to validate without changing code.
    }
);

if (!Glfw.Init())
{
    var errorCode = Glfw.GetError(out var errorMessage);
    throw new InvalidOperationException($"Failed to initialize GLFW: {errorCode}, {errorMessage}");
}

try
{
    Run();
}
finally
{
    Glfw.Terminate();
}

static void Run()
{
    using var window = Glfw.CreateWindow(
        new GlfwWindow.CreateInfo
        {
            Title = "Hello Glfw",
            Size = new int2(640, 480),
            Monitor = GlfwMonitor.Null, // Windowed mode.
            Share = null, // No shared context.
        }
    );
    window.OnKey += (handle, key, _, _, _) =>
    {
        if (key == GlfwKey.Escape)
            handle.ShouldClose = true;
    };
    while (!window.ShouldClose)
        Glfw.PollEvents();
}
/*
Glfw.WindowHint(GlfwWindowHint.ClientApi, 0);
var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo
{
    Title = "Hello BRUTAL",
    Size = new int2(1280, 720),
});

while (!window.ShouldClose)
    Glfw.PollEvents();

Glfw.Terminate();
*/