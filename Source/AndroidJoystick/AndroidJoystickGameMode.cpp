// Copyright Epic Games, Inc. All Rights Reserved.

#include "AndroidJoystickGameMode.h"
#include "AndroidJoystickCharacter.h"
#include "UObject/ConstructorHelpers.h"

AAndroidJoystickGameMode::AAndroidJoystickGameMode()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/ThirdPerson/Blueprints/BP_ThirdPersonCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}
