// Fill out your copyright notice in the Description page of Project Settings.


#include "AndroidInputGameInstance.h"

void UAndroidInputGameInstance::Init()
{


	Super::Init();
}

float UAndroidInputGameInstance::GetVallegRotationValue(APlayerController* pc)
{
	if (pc && pc->PlayerInput)
	{
		FKeyState* RotationKeyState = pc->PlayerInput->GetKeyState(FKey(TEXT("Gamepad_RightY")));
		if (RotationKeyState)
		{
			return RotationKeyState->RawValue.X;
		}
	
		for (auto& mapping : pc->PlayerInput->AxisMappings)
		{
			GEngine->AddOnScreenDebugMessage(-1, 0, FColor::Red, *mapping.Key.ToString());
		}

	}
	
	return 0.0f;
}
