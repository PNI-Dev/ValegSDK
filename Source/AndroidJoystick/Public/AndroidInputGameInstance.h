// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintPlatformLibrary.h"
#include "AndroidInputGameInstance.generated.h"

/**
 * 
 */
UCLASS()
class ANDROIDJOYSTICK_API UAndroidInputGameInstance : public UPlatformGameInstance
{

	GENERATED_BODY()

public:
	virtual void Init()override;

	UFUNCTION(BlueprintCallable, Category = GameInst)
	static float GetVallegRotationValue(APlayerController* pc);
};
