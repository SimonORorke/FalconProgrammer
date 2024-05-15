namespace FalconProgrammer.ViewModel;

public class MessageWindowViewModel {
  public MessageWindowViewModel(string text, string title) {
    Text = text;
    Title = title;
  }
  
  public string Text { get; }
  public string Title { get; }
}