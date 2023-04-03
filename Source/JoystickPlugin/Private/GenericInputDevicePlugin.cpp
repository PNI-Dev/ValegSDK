// Fill out your copyright notice in the Description page of Project Settings.


#include "GenericInputDevicePlugin.h"
#include "GenericPlatform/GenericApplicationMessageHandler.h"
#include "GenericPlatform/IInputInterface.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/GameViewportClient.h"

#define MAX_NUM_CONTROLLERS					8  // reasonable limit for now
#define MAX_NUM_PHYSICAL_CONTROLLER_BUTTONS	18
#define MAX_NUM_VIRTUAL_CONTROLLER_BUTTONS	8
#define MAX_NUM_CONTROLLER_BUTTONS			MAX_NUM_PHYSICAL_CONTROLLER_BUTTONS + MAX_NUM_VIRTUAL_CONTROLLER_BUTTONS
#define MAX_DEFERRED_MESSAGE_QUEUE_SIZE		128

GenericInputDevicePlugin::GenericInputDevicePlugin()
{

}

GenericInputDevicePlugin::~GenericInputDevicePlugin()
{
}

#if PLATFORM_ANDROID
extern bool AndroidThunkCpp_GetInputDeviceInfo(int32 deviceId, FAndroidInputDeviceInfo& results);
#endif


FGenericInputDevice::FGenericInputDevice(const TSharedRef<FGenericApplicationMessageHandler>& InMessageHandler) :
    MessageHandler(InMessageHandler)
{
    // Initiate your device here
	UE_LOG(LogTemp, Log, TEXT("Initial GenericInputDevice"));
}

FGenericInputDevice::~FGenericInputDevice()
{
    // Close your device here
}

void FGenericInputDevice::Tick(float DeltaTime)
{
    // Nothing necessary to do (boilerplate code to complete the interface)
}

void FGenericInputDevice::SendControllerEvents()
{
	
	UWorld* myWorld = GWorld->GetWorld();

	/*if (myWorld)
	{
		if (!bBindingKeys)
		{
			myWorld->GetGameViewport()->OnInputAxis().AddLambda([this](FViewport* InViewport, int32 ControllerId, FKey Key, float Delta, float DeltaTime, int32 NumSamples, bool bGamepad){
				OnInputAxisGenericDevice(InViewport, ControllerId, Key, Delta, DeltaTime, NumSamples, bGamepad);
			});
			myWorld->GetGameViewport()->OnInputKey().AddLambda([this](FInputKeyEventArgs EventArgs) {
				OnInputKeyGenericDevice(EventArgs);
			});
			
			bBindingKeys = true;
		}
	}*/

#if PLATFORM_ANDROID
	//UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] SendControllerEvents"));
	FAndroidApplication* app = FAndroidApplication::Get();
	if (app)
	{
		//UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] Valid Android Application"));
		IInputInterface* inputinterface = app->GetInputInterface();
		FAndroidInputInterface* input = static_cast<FAndroidInputInterface*>(inputinterface);
		if (input)
		{
			//UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] Valid Android Application Input Interface"));
			//UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] Start Found Gamepad"));

			for (int32 DeviceIndex = 0; DeviceIndex < MAX_NUM_CONTROLLERS; DeviceIndex++)
			{
				FAndroidGamepadDeviceMapping* CurrentDevice = input->GetDeviceMapping(DeviceIndex);

				FString DebugMsg = TEXT("CurrentDevice id : ");
				DebugMsg.Append(FString::FromInt(CurrentDevice->DeviceInfo.DeviceId));
				DebugMsg.Append(TEXT(", name : "));
				DebugMsg.Append(CurrentDevice->DeviceInfo.Name);
				UE_LOG(LogTemp, Log, TEXT("%s"), *DebugMsg);

				if (CurrentDevice->DeviceInfo.Name.Contains(TEXT("nJoys")))
				{
					//UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] Found nJoys Gamepad"));

					//MessageHandler->OnControllerAnalog(FGamepadKeyNames::SpecialLeft_X, CurrentDevice.DeviceInfo.ControllerId, 0.5f);
					//MessageHandler->OnControllerAnalog(FGamepadKeyNames::RightTriggerAnalog, CurrentDevice.DeviceInfo.ControllerId, 0.8f);

					if (!CurrentDevice->bRightStickRXRY)
					{
						UE_LOG(LogTemp, Log, TEXT("[PNIINPUT] Found And ReSetting nJoys Use RightStickRXRY"));
						CurrentDevice->bRightStickZRZ = false;
						CurrentDevice->bMapZRZToTriggers = false;
						CurrentDevice->bRightStickRXRY = true;
						CurrentDevice->bMapRXRYToTriggers = false;
					}

				}

			}
		}
		else
		{
			UE_LOG(LogTemp, Log, TEXT("UnValid Android Application Input Interface"));
		}
	}

#endif
	//UE_LOG(LogTemp, Log, TEXT("SendControlEvents"));
	APlayerController* pc = UGameplayStatics::GetPlayerController(myWorld, 0);
	if (pc && pc->PlayerInput)
	{
		/*FInputAxisProperties GamePadRightXProperties;
		if (pc->PlayerInput->GetAxisProperties(FKey(TEXT("Gamepad_RightX")), GamePadRightXProperties))
		{
			GamePadRightXProperties.DeadZone = 0.0f;
			pc->PlayerInput->SetAxisProperties(FKey(TEXT("Gamepad_RightX")), GamePadRightXProperties);
		}*/

		FInputAxisProperties GamePadRightYProperties;
		if (pc->PlayerInput->GetAxisProperties(FKey(TEXT("Gamepad_RightY")), GamePadRightYProperties))
		{
			GamePadRightYProperties.DeadZone = 0.0f;
			pc->PlayerInput->SetAxisProperties(FKey(TEXT("Gamepad_RightY")), GamePadRightYProperties);
		}


		/*FKeyState* RotationKeyState = pc->PlayerInput->GetKeyState(FKey(TEXT("Gamepad_RightY")));
		if (RotationKeyState)
		{
			float rotation =  RotationKeyState->RawValue.X;

			float convertedRotation = rotation;
			float rotationmultiplier = 1.0f;
			if (rotation < 0)
				rotationmultiplier = -1.0f;

			convertedRotation = rotationmultiplier - convertedRotation;

			FAnalogInputEvent rotationInputEvent(FKey(TEXT("Gamepad_RightX")), FSlateApplication::Get().GetModifierKeys(), 0, true, 0, 0, convertedRotation);
			FSlateApplication::Get().ProcessAnalogInputEvent(rotationInputEvent);
		}*/
	}
}

void FGenericInputDevice::SetMessageHandler(const TSharedRef< FGenericApplicationMessageHandler >& InMessageHandler)
{
    MessageHandler = InMessageHandler;

}

bool FGenericInputDevice::Exec(UWorld* InWorld, const TCHAR* Cmd, FOutputDevice& Ar)
{
    // Nothing necessary to do (boilerplate code to complete the interface)
    return false;
}

bool FGenericInputDevice::OnControllerAnalog(FGamepadKeyNames::Type KeyName, int32 ControllerId, float AnalogValue)
{
	return false;
}

void FGenericInputDevice::SetChannelValue(int32 ControllerId, FForceFeedbackChannelType ChannelType, float Value)
{
    // Nothing necessary to do (boilerplate code to complete the interface)
	
}

void FGenericInputDevice::SetChannelValues(int32 ControllerId, const FForceFeedbackValues& values)
{
    // Nothing necessary to do (boilerplate code to complete the interface)
}

void FGenericInputDevice::OnInputKeyGenericDevice(FInputKeyEventArgs EventArgs)
{
}

void FGenericInputDevice::OnInputAxisGenericDevice(FViewport* InViewport, int32 ControllerId, FKey Key, float Delta, float DeltaTime, int32 NumSamples, bool bGamepad)
{
}
