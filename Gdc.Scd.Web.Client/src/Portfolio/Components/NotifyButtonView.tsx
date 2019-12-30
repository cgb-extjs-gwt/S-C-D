import * as React from "react";
import { Button, Container, Dialog } from "@extjs/ext-react";
import { NotifyGrid, NotifyGridProps, NotifyGridActions } from "./NotifyGrid";
import { shomMask, hideMask } from "../../Common/Helpers/MaskHelper";
import { NamedId } from "../../Common/States/CommonStates";
import { Store } from "../../Common/States/ExtStates";

export interface NotifyButtonActions {
    onDialogNotifyButtonClick?(selectedItems: NamedId<number>[], store: Store, notifyButton: NotifyButtonView)
}

export interface NotifyButtonProps extends NotifyButtonActions {
    isVisible: boolean
    dataLoadUrl: string
}

export interface NotifyButtonState {
    isVisibleNotifyWindow: boolean
}

export class NotifyButtonView extends React.PureComponent<NotifyButtonProps, NotifyButtonState> {
    private dialog;
    
    constructor(props: NotifyButtonProps) {
        super(props);

        this.state = {
            isVisibleNotifyWindow: false
        };
    }

    public render() {
        const { isVisible, dataLoadUrl } = this.props;
        const { isVisibleNotifyWindow } = this.state;

        return (
            <Container>
                <Button text="Notify" handler={this.showNotifyWindow} hidden={!isVisible}/>
                {
                    isVisibleNotifyWindow &&
                    <Dialog 
                        ref={this.setDialogRef}
                        displayed={isVisibleNotifyWindow} 
                        title="New Warranty groups" 
                        closable 
                        resizable={{
                            dynamic: true,
                            edges: 'all'
                        }}
                        width="50%"
                        height="60%"
                        layout="fit"
                        top="10%"
                        left="10%"
                        onClose={this.hideNotifyWindow}
                    >
                        <NotifyGrid dataLoadUrl={dataLoadUrl} onNotifyButtonClick={this.onDialogNotifyButtonClick}/>
                    </Dialog>
                }
            </Container>
        )
    }

    public showDialogMask() {
        shomMask(this.dialog);
    }

    public hideDialogMask() {
        hideMask(this.dialog);
    }

    private setDialogRef = dialog => {
        this.dialog = dialog;
    }

    private showNotifyWindow = () => {
        this.setState({ isVisibleNotifyWindow: true });
    }

    private hideNotifyWindow = () => {
        this.setState({ isVisibleNotifyWindow: false });
    }

    private onDialogNotifyButtonClick = (selectedItems: NamedId<number>[], store: Store) => {
        const { onDialogNotifyButtonClick } = this.props;

        onDialogNotifyButtonClick && onDialogNotifyButtonClick(selectedItems, store, this);
    }
}