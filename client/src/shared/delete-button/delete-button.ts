import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-delete-button',
  standalone: true,
  imports: [],
  templateUrl: './delete-button.html',
  styleUrls: ['./delete-button.css']
})
export class DeleteButton {
  disabled = input<boolean>();
  clickEvent = output<Event>();

  onClick(event: Event) {
    if (this.disabled && this.disabled()) return;
    this.clickEvent.emit(event);
  }
}