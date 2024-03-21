# Filesystem Specifications

## Overview

The Filesystem is a custom implementation designed to manage files and directories on a disk. It supports basic file operations such as creating, editing, and reading files, as well as managing directories. The filesystem is structured with an emphasis on efficiency and simplicity.

## Notes

- The filesystem is implemented in C#.
- It uses a custom entry structure to represent files and directories.
- The disk is divided into pages, each with a specified size.
- File allocation is managed using a File Allocation Table (FAT).
- The filesystem supports flags for directory, protection, hidden, and large file.

## Entries Layout

Each entry in the filesystem is structured as follows:

- ``00-0F`` Name (15 bytes)
- ``10-13`` Start Address (4 bytes)
- ``14-17`` File Length (4 bytes)
- ``18-18`` Flags (1 bytes)
- ``19-1A`` Entry Count (2 bytes)
- ``1B-1F`` Date more in the Date format section (5 bytes)

## First Page Layout

The first page of the disk contains essential information about the filesystem:

- ``000-004`` Disk Version (4 bytes)
- ``005-005`` Disk Letter (1 byte)
- ``006-006`` Unused (1 byte)
- ``007-008`` Free Pages (2 bytes)
- ``009-009`` Sectors Per Page (1 byte)
- ``00A-00B`` Page Size (2 bytes)
- ``100-4FF`` Root Directory Entries (variable)

## Page Layout (Split into 4 Sections)

a page is split into 4 different sections

- ``000-1FF`` sector 1 (512 byte)
- ``200-3FF`` sector 2 (512 byte)
- ``400-5FF`` sector 3 (512 byte)
- ``600-7FF`` sector 4 (512 byte)

## Flags

- Is Directory Flag: 0b0000000000000001
- Is Protected Flag: 0b0000000000000010
- Is Hidden Flag: 0b0000000000000100
- Is Bigger Than Flag: 0b0000000000001000

## Date format

dates are formated in a spicivfig way using only 5 bytes

0bUUUU_YYYY_YYYM_MMMD_DDDD

Day (5 bits 01 - 31)
month (4 bits 01 - 12)
year (7 bit 00 - 99 dec)
unused
