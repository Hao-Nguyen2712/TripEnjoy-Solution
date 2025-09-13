<script setup>
import { ref } from 'vue';
import heroImage from '@/assets/img/hero/hero-1.jpg';
import carouselImage1 from '@/assets/img/carousel/carousel-1.jpg';
import { TabGroup, TabList, Tab, TabPanels, TabPanel } from '@headlessui/vue'
const activeTab = ref('hotels');
</script>

<template>
  <div>
    <!-- Hero Section -->
    <section
      class="relative bg-cover bg-center text-white pt-48 pb-24"
      :style="{ backgroundImage: `url(${heroImage})` }">
      <div class="absolute inset-0 bg-black opacity-50"></div>
      <div class="relative max-w-7xl mx-auto px-4 text-center">
          <div class="grid grid-cols-12 gap-8 items-center">
            <div class="col-span-12 lg:col-span-7 text-left">
              <h1 class="text-5xl md:text-7xl font-bold mb-4 leading-tight">Experience the Magic: Exclusive Offers from TripEnjoy</h1>
              <p class="text-xl mb-8">Your ultimate destination for all things help you celebrate & remember tour experience.</p>
              <a href="#" class="bg-red-500 text-white font-semibold px-8 py-3 rounded-full hover:bg-red-600 transition-colors text-lg">Explore All Hotels</a>
            </div>
            <div class="col-span-12 lg:col-span-5">
              <!-- Image Collage can be added here if needed -->
            </div>
        </div>
      </div>
    </section>

    <!-- Booking Form Section -->
    <section class="relative -mt-16">
      <div class="max-w-6xl mx-auto px-4">
        <div class="bg-white rounded-lg shadow-lg p-8">
            <TabGroup>
                <TabList class="flex space-x-1 rounded-xl bg-gray-200 p-1 mb-6">
                     <Tab as="template" v-slot="{ selected }">
                        <button :class="[
                            'w-full rounded-lg py-2.5 text-sm font-medium leading-5',
                            'ring-white ring-opacity-60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2',
                            selected
                            ? 'bg-white shadow text-red-500'
                            : 'text-gray-700 hover:bg-white/[0.5] hover:text-red-400'
                        ]">
                        <i class="fas fa-plane mr-2"></i> Flights
                        </button>
                    </Tab>
                    <Tab as="template" v-slot="{ selected }">
                        <button :class="[
                            'w-full rounded-lg py-2.5 text-sm font-medium leading-5',
                            'ring-white ring-opacity-60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2',
                            selected
                            ? 'bg-white shadow text-red-500'
                            : 'text-gray-700 hover:bg-white/[0.5] hover:text-red-400'
                        ]">
                        <i class="fas fa-hotel mr-2"></i> Hotels
                        </button>
                    </Tab>
                    <Tab as="template" v-slot="{ selected }">
                        <button :class="[
                            'w-full rounded-lg py-2.5 text-sm font-medium leading-5',
                            'ring-white ring-opacity-60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2',
                            selected
                            ? 'bg-white shadow text-red-500'
                            : 'text-gray-700 hover:bg-white/[0.5] hover:text-red-400'
                        ]">
                        <i class="fas fa-car mr-2"></i> Cars
                        </button>
                    </Tab>
                    <Tab as="template" v-slot="{ selected }">
                        <button :class="[
                            'w-full rounded-lg py-2.5 text-sm font-medium leading-5',
                            'ring-white ring-opacity-60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2',
                            selected
                            ? 'bg-white shadow text-red-500'
                            : 'text-gray-700 hover:bg-white/[0.5] hover:text-red-400'
                        ]">
                        <i class="fas fa-ship mr-2"></i> Cruise
                        </button>
                    </Tab>
                    <Tab as="template" v-slot="{ selected }">
                        <button :class="[
                            'w-full rounded-lg py-2.5 text-sm font-medium leading-5',
                            'ring-white ring-opacity-60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2',
                            selected
                            ? 'bg-white shadow text-red-500'
                            : 'text-gray-700 hover:bg-white/[0.5] hover:text-red-400'
                        ]">
                        <i class="fas fa-route mr-2"></i> Tour
                        </button>
                    </Tab>
                </TabList>
                <TabPanels>
                    <TabPanel>
                        <!-- Flights booking form -->
                         <form class="grid grid-cols-1 md:grid-cols-5 gap-4 items-end">
                            <div class="md:col-span-2">
                                <label for="flight-destination" class="block text-sm font-medium text-gray-700">City, Property name or Location</label>
                                <input type="text" id="flight-destination" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500" placeholder="Newyork">
                            </div>
                            <div>
                                <label for="flight-checkin" class="block text-sm font-medium text-gray-700">Check In</label>
                                <input type="date" id="flight-checkin" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500">
                            </div>
                            <div>
                                <label for="flight-checkout" class="block text-sm font-medium text-gray-700">Check Out</label>
                                <input type="date" id="flight-checkout" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500">
                            </div>
                             <button class="bg-red-500 text-white font-semibold rounded-lg px-6 py-3 hover:bg-red-600 transition-colors w-full">Search</button>
                        </form>
                    </TabPanel>
                    <TabPanel>
                         <form class="grid grid-cols-1 md:grid-cols-5 gap-4 items-end">
                            <div>
                                <label for="hotel-destination" class="block text-sm font-medium text-gray-700">City, Property name or Location</label>
                                <input type="text" id="hotel-destination" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500" placeholder="Newyork">
                            </div>
                            <div>
                                <label for="hotel-checkin" class="block text-sm font-medium text-gray-700">Check In</label>
                                <input type="date" id="hotel-checkin" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500">
                            </div>
                            <div>
                                <label for="hotel-checkout" class="block text-sm font-medium text-gray-700">Check Out</label>
                                <input type="date" id="hotel-checkout" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500">
                            </div>
                            <div>
                                <label for="hotel-guests" class="block text-sm font-medium text-gray-700">Guests</label>
                                <input type="number" id="hotel-guests" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500" placeholder="4">
                            </div>
                             <button class="bg-red-500 text-white font-semibold rounded-lg px-6 py-3 hover:bg-red-600 transition-colors w-full">Search</button>
                        </form>
                    </TabPanel>
                    <!-- Other tab panels -->
                </TabPanels>
            </TabGroup>
        </div>
      </div>
    </section>
  </div>
</template>
