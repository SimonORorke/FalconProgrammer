using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Combines the functionality of <see cref="ObservableValidator" /> with the basics of
///   <see cref="ObservableRecipient" />. 
/// </summary>
/// <param name="messenger"></param>
public abstract class ObservableRecipientWithValidation(IMessenger messenger)
  : ObservableValidator {
  protected ObservableRecipientWithValidation()
    : this(WeakReferenceMessenger.Default) { }

  protected IMessenger Messenger { get; } = messenger;
}