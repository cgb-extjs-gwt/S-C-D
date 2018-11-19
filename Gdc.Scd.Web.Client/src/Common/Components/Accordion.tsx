import * as React from "react";

export interface AccordionProps {
    title: React.ReactNode;
    onExpand?: (me: any) => void;
    onCollapse?: (me: any) => void;
}

export class Accordion extends React.Component<AccordionProps, any> {

    private header: HTMLElement;

    private body: HTMLElement;

    public render() {
        return (
            <div className="accordion-block">
                <div ref={x => this.header = x} className="accordion" onClick={x => this.onHeaderClick()}>
                    {this.props.title}
                </div>
                <div ref={x => this.body = x} className="accordion-panel">
                    {this.props.children}
                </div>
            </div>
        );
    }

    private onHeaderClick() {
        let cls = 'accordion-active';

        this.header.classList.toggle(cls);

        if (this.header.classList.contains(cls)) {
            if (this.props.onExpand) {
                this.props.onExpand(this);
            }
        } else {
            if (this.props.onCollapse) {
                this.props.onCollapse(this);
            }
        }
    }
}