using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class MockMessageRecipient : ObservableRecipient,
  IRecipient<GoToLocationsPageMessage> {
  public MockMessageRecipient() {
    IsActive = true; // Start listening for ObservableRecipient messages.
  }

  [PublicAPI] internal int GoToLocationsPageCount { get; set; }

  public void Receive(GoToLocationsPageMessage message) {
    GoToLocationsPageCount++;
  }
}