import { DictFilter, DictFilterProps } from "./DictFilter";

export class SogFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getSog()
        });
    }
}