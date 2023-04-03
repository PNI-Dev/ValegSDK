// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class AndroidJoystick : ModuleRules
{
	public AndroidJoystick(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "HeadMountedDisplay"});

		//PublicAdditionalLibraries.Add(Path.Combine(ModuleDirectory, "thirdparty", "libpaddleboat_static.a"));
	}
}
