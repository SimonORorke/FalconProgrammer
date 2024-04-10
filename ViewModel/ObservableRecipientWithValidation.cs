using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Combines the functionality of <see cref="ObservableValidator" /> with the basics of
///   <see cref="ObservableRecipient" />.
/// </summary>
/// <param name="messenger">
///   A mock messenger can be specified for testing. Otherwise, the protected constructor
///   will set the <see cref="Messenger" /> to the default
///   <see cref="WeakReferenceMessenger" />.
/// </param>
/// <remarks>
///   As we have not implemented the functionality of
///   <see cref="ObservableRecipient.IsActive" />, <see cref="ViewModelBase" />
///   registers and unregisters with the <see cref="Messenger" />. So recipient view
///   models, which implement IRecipient for one or more message types, don't have to
///   do that individually.
/// </remarks>
public abstract class ObservableRecipientWithValidation(IMessenger messenger)
  : ObservableValidator {
  protected ObservableRecipientWithValidation()
    : this(WeakReferenceMessenger.Default) { }

  protected IMessenger Messenger { get; } = messenger;
}