import { shomMask, hideMask } from "./MaskHelper";

export const handleRequest = (promise: Promise<any>) => {
    shomMask();

    return promise

        .then(() => hideMask()) //fix, there is strange bug with hide musk, may be it's a bug with typescript compilation/optimization

        .catch(error => {
            hideMask();
            Ext.Msg.alert('Error', 'Request failed');
            console.error(error);
        });
}