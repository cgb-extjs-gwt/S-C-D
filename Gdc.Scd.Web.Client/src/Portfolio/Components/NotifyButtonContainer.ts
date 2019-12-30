import { connect } from "react-redux";
import { NotifyButtonProps, NotifyButtonView, NotifyButtonActions } from "./NotifyButtonView";
import { CommonState } from "../../Layout/States/AppStates";
import * as Permissions from "../../Common/Constants/Permissions";
import { PortfolioService } from "../Services/PortfolioService";

const portfolioService = new PortfolioService();

export const NotifyButtonContainer = connect<NotifyButtonProps, NotifyButtonActions, {}, CommonState>(
    ({ app }) => ({
        isVisible: !!app.userRoles.find(userRole => userRole.permissions.includes(Permissions.ADMIN)),
        dataLoadUrl: portfolioService.buildGetNewWgsUrl()
    }),
    dispatch => ({
        onDialogNotifyButtonClick: (selectedItems, store, notifyButton) => {
            if (selectedItems.length == 0) {
                Ext.Msg.alert('Message', 'Please select warranty groups');
            } else {
                notifyButton.showDialogMask();

                portfolioService.notifyCountryUsers(selectedItems).then(
                    () => {
                        notifyButton.hideDialogMask();
                        Ext.Msg.alert('Message', 'Notification successful');
                        store.reload();
                    },
                    () => {
                        notifyButton.hideDialogMask();
                        Ext.Msg.alert('Error', 'An error occurred during the notification');
                    }
                );
            }
        }
    })
)(NotifyButtonView)