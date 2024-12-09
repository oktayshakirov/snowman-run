#import <AVFoundation/AVFoundation.h>

extern "C" {
    void SetAVAudioSessionPlayback() {
        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback error:nil];
        [[AVAudioSession sharedInstance] setActive:YES error:nil];
    }
}