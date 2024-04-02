using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   An ObservableRecipient message, received by <see cref="MainWindowViewModel" />,
///   asking for the Locations page to be shown instead of the current page.
/// </summary>
/// <remarks>
///   Kevin Bost recommends using a record class for an ObservableRecipient message.
///   See https://www.youtube.com/watch?v=bCryIp9HqIM, 1:48:57 for his more fullsome
///   example.
/// </remarks>
[SuppressMessage("ReSharper", "CommentTypo")]
public record GoToLocationsPageMessage;