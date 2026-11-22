const status = document.getElementById("status");

try {
    const { dotnet } = await import("./_framework/dotnet.js");
    const runtime = await dotnet.create();
    await runtime.runMain(runtime.getConfig().mainAssemblyName, []);
    status.remove();
} catch (error) {
    status.textContent = "The Teacups UI demo could not start.";
    console.error(error);
}
