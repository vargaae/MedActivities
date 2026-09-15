import assert from 'node:assert/strict';
import { test } from 'node:test';
import { formatDateOnly } from '../src/lib/util/util.ts';

test('missing and invalid birth dates render a placeholder without throwing', () => {
  for (const value of ['', '   ', null, undefined, 'not-a-date', '2026-02-30', new Date(NaN)]) {
    assert.equal(formatDateOnly(value), '—');
  }
});

test('valid birth dates keep Hungarian formatting', () => {
  assert.equal(formatDateOnly('1990-02-03'), '1990.02.03.');
  assert.equal(formatDateOnly('2024-02-29'), '2024.02.29.');
  assert.equal(formatDateOnly(new Date(1990, 1, 3)), '1990.02.03.');
});
