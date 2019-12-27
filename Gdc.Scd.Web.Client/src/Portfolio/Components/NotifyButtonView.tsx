import * as React from "react";
import { Button, Container, Dialog } from "@extjs/ext-react";
import { NotifyGrid, NotifyGridProps, NotifyGridActions } from "./NotifyGrid";

export interface NotifyButtonProps extends NotifyGridProps, NotifyGridActions {
    isVisible: boolean
}

export interface NotifyButtonState {
    isVisibleNotifyWindow: boolean
}

export class NotifyButtonView extends React.PureComponent<NotifyButtonProps, NotifyButtonState> {
    constructor(props: NotifyButtonProps) {
        super(props);

        this.state = {
            isVisibleNotifyWindow: false
        };
    }

    public render() {
        const { isVisible } = this.props;
        const { isVisibleNotifyWindow } = this.state;

        return (
            <Container>
                <Button text="Notify" handler={this.showNotifyWindow} hidden={!isVisible}/>
                {
                    isVisibleNotifyWindow &&
                    <Dialog 
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
                        <NotifyGrid {...this.props}/>
                    </Dialog>
                }
            </Container>
        )
    }

    private showNotifyWindow = () => {
        this.setState({ isVisibleNotifyWindow: true });
    }

    private hideNotifyWindow = () => {
        this.setState({ isVisibleNotifyWindow: false });
    }
}