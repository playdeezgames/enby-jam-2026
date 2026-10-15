package gwen

import "core:unicode/utf8"

// Encodes UTF-8 into IBM code page 437 bytes (what the DINOBYTE sheet is laid out in).
// Unknown characters become '?'. Returns the number of bytes written.
cp437_encode :: proc(dst: []u8, s: string) -> int {
	n := 0
	for r in s {
		if n >= len(dst) {
			break
		}
		dst[n] = cp437_from_rune(r)
		n += 1
	}
	return n
}

cp437_from_rune :: proc(r: rune) -> u8 {
	if r >= 32 && r < 127 {
		return u8(r)
	}
	switch r {
	case 'Ç': return 0x80
	case 'ü': return 0x81
	case 'é': return 0x82
	case 'â': return 0x83
	case 'ä': return 0x84
	case 'à': return 0x85
	case 'å': return 0x86
	case 'ç': return 0x87
	case 'ê': return 0x88
	case 'ë': return 0x89
	case 'è': return 0x8A
	case 'ï': return 0x8B
	case 'î': return 0x8C
	case 'ì': return 0x8D
	case 'Ä': return 0x8E
	case 'Å': return 0x8F
	case 'É': return 0x90
	case 'ö': return 0x94
	case 'ò': return 0x95
	case 'û': return 0x96
	case 'ù': return 0x97
	case 'ÿ': return 0x98
	case 'Ö': return 0x99
	case 'Ü': return 0x9A
	case 'ñ': return 0xA4
	case 'Ñ': return 0xA5
	case '’': return '\''
	case '‘': return '\''
	case '“': return '"'
	case '”': return '"'
	case '…': return '.'
	case '–', '—': return '-'
	}
	return '?'
}

// Finds one wrapped line starting at start. Breaks at the last space that fits, or splits a
// word that is longer than the line. Returns the end of the line text and where the next starts.
wrap_line :: proc(text: []u8, start, width: int) -> (end, next: int) {
	n := len(text)
	if n - start <= width {
		return n, n
	}
	i := start + width
	for i > start && text[i] != ' ' {
		i -= 1
	}
	if i == start {
		return start + width, start + width
	}
	end = i
	next = i + 1
	for next < n && text[next] == ' ' && next - start < width {
		next += 1
	}
	return end, next
}

utf8_len :: proc(s: string) -> int {
	return utf8.rune_count_in_string(s)
}
