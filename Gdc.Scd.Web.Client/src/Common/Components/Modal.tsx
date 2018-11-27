import * as React from "react";

export interface ModalProps {

    displayed?: boolean;

    title: React.ReactNode;

    width?: string;
    heigth?: string;

    onClose?: (me: any) => void;
}

export class Modal extends React.Component<ModalProps, any> {

    private dlg: HTMLElement;

    public render() {
        let cls = this.props.displayed ? 'modal modal-open' : 'modal';
        console.log('Modal.render()', cls);
        return (
            <div ref={x => this.dlg = x} className={cls}>

                <span className="modal-close" onClick={x => this.close()} >&times;</span>

                <div className="modal-content">
                    <div className="modal-header">
                        {this.props.title}
                    </div>
                    <div className="modal-body">
                        {this.props.children}
                    </div>
                </div>

            </div>
        );
    }

    public show() {
        this.dlg.classList.add('modal-open');
    }

    public close() {
        this.dlg.classList.remove('modal-open');
        if (this.props.onClose) {
            this.props.onClose(this);
        }
    }
}