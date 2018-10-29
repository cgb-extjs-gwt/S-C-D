import { WebworkerHelper } from "../Common/Helpers/WebworkerHelper";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { APP_ALERT_DEFAULT } from "../Layout/Actions/AlertActions";

/** 
 * HTML5 web workder task
 * implement infinite long polling connection
*/
const connect = (function () {

    const _baseurl = window.location.protocol + '//' + window.location.host + buildMvcUrl('notify', 'connect');

    //return function src as string!!! and replace server '_baseurl'

    return function connect() {
        let last_index = 0;
        let url = '_baseurl?_dc=' + new Date().getTime();
        try {
            let xhr = new XMLHttpRequest();
            xhr.open('get', url, true);
            xhr.onprogress = function () {

                let curr_index = xhr.responseText.length;
                if (last_index == curr_index) {
                    return;
                }

                //get new string block

                let batch = xhr.responseText.substring(last_index, curr_index);
                last_index = curr_index;

                //important to split server answer to get valid json packages
                let messages = batch.split('\n---\n');

                //ok, parse json and post messages to main thread

                for (let i = 0, len = messages.length; i < len; i++) {

                    let s = messages[i];

                    if (!s) {
                        continue;
                    }

                    try {
                        let data = JSON.parse(s);

                        if (data.type !== '<HELLO>') {
                            self.postMessage(data, null); //ignore 'hello' server answer, post only significant data
                        }
                    }
                    catch (e) {
                        console.log(e, s);
                    }

                }
            };
            xhr.onreadystatechange = function (e) {

                if (xhr.readyState != 4) {
                    return;
                }

                //request finished, reconnect

                if (xhr.status == 200) {
                    connect();
                }
                else {
                    setTimeout(connect, 60000); //1 min wait before try reconnect...
                }
            };
            xhr.send();
        }
        catch (e) {
            console.log(e);
        }
    }.toString().replace('_baseurl', _baseurl);

})();

function fakeConnect() {
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
}

let instance: Worker;

export function RemoteNotify(dispatch) {
    if (!instance) {
        //instance = WebworkerHelper.run(connect);
        instance = WebworkerHelper.run(fakeConnect);

        //listen to worker answer and dispatch event to main window
        instance.onmessage = function (e: MessageEvent) {
            let d = e.data;
            if (d.type) {
                d.type = d.type.toUpperCase();
            }
            else {
                d.type = APP_ALERT_DEFAULT; //unknown message prepare as default
            }
            dispatch(d);
        };
    }
    return instance;
};
