import * as React from "react";
import { Dispatch } from "redux";
import { connect } from "react-redux";
import { CapabilityMatrixEditView, CapabilityMatrixEditViewProps } from "../Components/CapabilityMatrixEditView";
import { NamedId } from "../../Common/States/CommonStates";
import { allowCombination, denyCombination, countryChange } from "../Actions/CapabilityMatrixActions";
import { ExtMsgHelper } from "../../Common/Helpers/ExtMsgHelper";

function mapStateToProps(state: any): CapabilityMatrixEditViewProps {

    let store = [
        { name: 'Item 1' },
        { name: 'Item 2' },
        { name: 'Item 3' },
        { name: 'Item 4' }
    ] as NamedId[];

    let countries = [
        { "name": "Alabama", "id": "AL" },
        { "name": "Alaska", "id": "AK" },
        { "name": "Arizona", "id": "AZ" }
    ] as NamedId[];

    return {

        countries: countries,

        isPortfolio: false,

        availabilityTypes: store,

        durationTypes: store,

        reactionTimeTypes: store,

        reactTypes: store,

        serviceLocationTypes: store,

        warrantyGroups: store

    } as CapabilityMatrixEditViewProps;
}

function mapDispatchToProps(dispatch: Dispatch) {
    return {
        onCountryChange(newVal: string, oldVal: string) {
            dispatch(countryChange());
        },

        onAllow() {
            showSaveDialog('Allow combinations', () => {
                dispatch(allowCombination());
            });

        },

        onDeny() {
            showSaveDialog('Deny combinations', () => {
                dispatch(denyCombination());
            });
        }
    };
}

function showSaveDialog(title: string, ok: Function) {
    ExtMsgHelper.confirm(title, 'Do you want to save the changes?', ok);
}

export const CapabilityMatrixEditContainer = connect(mapStateToProps, mapDispatchToProps)(CapabilityMatrixEditView);
