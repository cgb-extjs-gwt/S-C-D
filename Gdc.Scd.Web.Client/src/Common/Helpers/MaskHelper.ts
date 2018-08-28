import { ROOT_LAYOUT_ID } from "../../Layout/Components/Layout";

const getLayout = () => (<any>Ext.getCmp(ROOT_LAYOUT_ID));

export const shomMask = () => {
    const layout = getLayout();

    layout && layout.setMasked({xtype: "loadmask"});
}

export const hideMask = () => {
    const layout = getLayout();

    layout && layout.setMasked(false);
}