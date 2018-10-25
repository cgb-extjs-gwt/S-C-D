import { buildMvcUrl } from "../Common/Services/Ajax";
import { WebworkerHelper } from "../Common/Helpers/WebworkerHelper";

//HTML5 web workder task

function connect() {

    //implement infinite long polling connection

    let last_index = 0;
    let url = buildMvcUrl('notify', 'connect', { _dc: new Date().getTime() });

    let xhr = new XMLHttpRequest();
    xhr.open('get', url, true);
    xhr.onprogress = function () {

        //get new string block
        //and parse json

        let curr_index = xhr.responseText.length;
        if (last_index == curr_index) {
            return;
        }

        let s = xhr.responseText.substring(last_index, curr_index);
        last_index = curr_index;

        try {
            let data = JSON.parse(s);
            self.postMessage(data, null);
        }
        catch (e) {
            console.log(e);
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
};

export function notifyWorker() {
    return WebworkerHelper.run(connect);
};
