#import <UIKit/UIKit.h>

extern "C" {
    void TriggerHapticFeedback(int type) {
        if (@available(iOS 10.0, *)) {
            if (type == 0) { // Light impact
                UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                [feedback impactOccurred];
            } else if (type == 1) { // Medium impact
                UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [feedback impactOccurred];
            } else if (type == 2) { // Heavy impact
                UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                [feedback impactOccurred];
            } else if (type == 3) { // Selection feedback
                UISelectionFeedbackGenerator *feedback = [[UISelectionFeedbackGenerator alloc] init];
                [feedback selectionChanged];
            } else if (type == 4) { // Notification success
                UINotificationFeedbackGenerator *feedback = [[UINotificationFeedbackGenerator alloc] init];
                [feedback notificationOccurred:UINotificationFeedbackTypeSuccess];
            } else if (type == 5) { // Notification warning
                UINotificationFeedbackGenerator *feedback = [[UINotificationFeedbackGenerator alloc] init];
                [feedback notificationOccurred:UINotificationFeedbackTypeWarning];
            } else if (type == 6) { // Notification error
                UINotificationFeedbackGenerator *feedback = [[UINotificationFeedbackGenerator alloc] init];
                [feedback notificationOccurred:UINotificationFeedbackTypeError];
            }
        }
    }
}