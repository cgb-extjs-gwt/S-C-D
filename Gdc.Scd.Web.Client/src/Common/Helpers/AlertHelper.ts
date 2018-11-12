import { AlertAction, report } from "../../Layout/Actions/AlertActions";

export class AlertHelper {

    private static dispatch: Function;

    public static init(dispatch: Function) {
        this.dispatch = dispatch;
    }

    public static autoload(url: string) {
        this.send(report('Report', 'Your report in process... Please wait while it finish', url));
    }

    private static send(msg: AlertAction) {
        if (this.dispatch) {
            this.dispatch(msg);
        }
    }
}
