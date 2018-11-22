import { AppConstantsModule } from './app-constants.module';

describe('AppConstantsModule', () => {
  let appConstantsModule: AppConstantsModule;

  beforeEach(() => {
    appConstantsModule = new AppConstantsModule();
  });

  it('should create an instance', () => {
    expect(appConstantsModule).toBeTruthy();
  });
});
