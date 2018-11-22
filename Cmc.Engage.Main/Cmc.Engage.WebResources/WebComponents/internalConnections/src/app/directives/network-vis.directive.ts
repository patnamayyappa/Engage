import { Directive, Input, ElementRef } from "@angular/core";
import {Network} from "vis";
 
@Directive({
  selector: "[appNetworkVis]"
})
export class NetworkVisDirective {
  network: Network;

  constructor(private el: ElementRef) {}

  @Input()
  set appNetworkVis(networkDetails) {
    if (!this.network) {
      if (networkDetails) {
        this.network = new Network(this.el.nativeElement, networkDetails.networkData, networkDetails.options);
      } else {
        const defaultOptions = {};
        this.network = new Network(this.el.nativeElement, null, defaultOptions);
      }
    } else {
      this.network.setData(networkDetails.networkData);
      this.network.setOptions(networkDetails.options);
    }
  }
}
