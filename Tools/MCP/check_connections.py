"""Verifica os dois MCPs sem alterar cenas. Execute com Python e o pacote mcp."""

import asyncio
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT = Path(__file__).resolve().parents[2]


async def inspect(command, args, tool, arguments, expected_path):
    params = StdioServerParameters(command=command, args=args)
    async with stdio_client(params) as (reader, writer):
        async with ClientSession(reader, writer) as session:
            info = await session.initialize()
            tools = (await session.list_tools()).tools
            if tool not in {item.name for item in tools}:
                raise RuntimeError(f"{info.serverInfo.name}: ferramenta {tool} indisponível")
            result = await session.call_tool(tool, arguments)
            payload = result.model_dump(mode="json", exclude_none=True)
            if result.isError or str(expected_path) not in json.dumps(payload, ensure_ascii=False):
                raise RuntimeError(f"Falha ou projeto incorreto em {tool}: {payload}")
            return {
                "server": info.serverInfo.model_dump(mode="json", exclude_none=True),
                "tool_count": len(tools),
                "tool": tool,
                "response": payload,
            }


async def main():
    unity = shutil.which("unity") or str(Path.home() / ".unity/bin/unity")
    blender = shutil.which("blender-mcp") or str(Path.home() / ".local/bin/blender-mcp")
    report = {"checked_at_utc": datetime.now(timezone.utc).isoformat()}
    report["unity"] = await inspect(
        unity,
        ["mcp", "--project-path", str(ROOT / "Freyja")],
        "eval",
        {"code": "return new { project = UnityEngine.Application.dataPath, version = UnityEngine.Application.unityVersion, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, compiling = UnityEditor.EditorApplication.isCompiling, updating = UnityEditor.EditorApplication.isUpdating, renderPipeline = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null ? \"Built-in\" : UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().Name };"},
        ROOT / "Freyja/Assets",
    )
    report["blender"] = await inspect(
        blender,
        [],
        "execute_blender_code",
        {"code": "import bpy\nresult = {'file': bpy.data.filepath, 'version': bpy.app.version_string, 'scene': bpy.context.scene.name, 'dirty': bpy.data.is_dirty}"},
        ROOT / "Source/Blender/Cais_Iteracao01.blend",
    )
    destination = ROOT / "Tools/MCP/connection-status.json"
    destination.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
    print(f"Unity e Blender responderam via MCP no projeto correto. Evidência: {destination}")


if __name__ == "__main__":
    asyncio.run(asyncio.wait_for(main(), timeout=60))
