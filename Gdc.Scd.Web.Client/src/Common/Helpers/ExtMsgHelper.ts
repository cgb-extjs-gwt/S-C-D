export class ExtMsgHelper {

    public static isOk(btn: string): boolean {
        return btn === 'yes';
    }

    public static ok(fn: Function) {

        if (!fn) {
            throw new Error('Invalid ok handler');
        }

        return (yesno) => {
            if (this.isOk(yesno)) {
                fn();
            }
        };
    }

    public static confirm(title: string, text: string, ok: Function) {
        Ext.Msg.confirm(title, text, ExtMsgHelper.ok(ok));
    }

}