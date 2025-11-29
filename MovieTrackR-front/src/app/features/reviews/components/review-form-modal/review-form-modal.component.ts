import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SliderModule } from 'primeng/slider';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-review-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, TextareaModule, SliderModule],
  templateUrl: './review-form-modal.component.html',
  styleUrl: './review-form-modal.component.scss',
})
export class ReviewFormModalComponent {

}
