using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockBatchLog : SubscribableBatchLog {
  internal string Text => ToString();
}