export function fakeConnect() {
    setInterval(function () {

        let msg: any = { type: '', text: '' };
        let r = Math.random();

        if (r > 0.85) {
            msg.type = 'APP.ALERT.DEFAULT';
            msg.text = 'Remote msg received'
        }
        else if (r > 0.67) {
            msg.type = 'APP.ALERT.ERROR';
            msg.text = 'Danger remote msg received'
        }
        else if (r > 0.55) {
            msg.type = 'APP.ALERT.SUCCESS';
            msg.text = 'Default remote msg received'
        }
        else if (r > 0.45) {
            msg.type = 'APP.ALERT.INFO';
            msg.text = 'Basic information remote msg received'
        }
        else if (r > 0.3) {
            msg.type = 'APP.ALERT.WARNING';
            msg.text = 'Warning remote message received'
        }
        else if (r > 0.15) {
            msg.type = 'APP.ALERT.REPORT';
            msg.text = 'Your report is prepared! Autodownload starting...'
            //msg.url = 'http://jqueryui.com/resources/download/jquery-ui-1.12.1.zip';
        }
        else {
            msg.type = 'APP.ALERT.LINK';
            msg.text = 'Your link to resource is prepared!'
            msg.url = 'https://www.w3schools.com/howto/tryit.asp?filename=tryhow_js_alerts';
        }

        self.postMessage(msg, null);
    }, 5000);
};