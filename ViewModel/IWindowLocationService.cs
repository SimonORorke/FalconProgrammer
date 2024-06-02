namespace FalconProgrammer.ViewModel;

public interface IWindowLocationService {
  int? Left { get; set; }
  int? Top { get; set; }
  int? Width { get; set; }
  int? Height { get; set; }
  int? WindowState { get; set; }
  void Restore();
  void Update();
}